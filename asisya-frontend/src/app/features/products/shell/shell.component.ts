import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-shell',
  standalone: false,
  templateUrl: './shell.component.html'
})
export class ShellComponent {
  constructor(public authService: AuthService, public router: Router) {}

  get tokenSnippet(): string {
    const t = this.authService.getToken();
    return t ? t.slice(0, 6) + '…' + t.slice(-4) : '—';
  }

  get crumbs(): string[] {
    const url = this.router.url;
    if (url.startsWith('/products/categories/new')) return ['Catálogo', 'Categorías', 'Nueva'];
    if (url.startsWith('/products/categories'))     return ['Catálogo', 'Categorías'];
    if (url.startsWith('/products/new'))            return ['Catálogo', 'Productos', 'Nuevo'];
    if (url.startsWith('/products/edit'))           return ['Catálogo', 'Productos', 'Editar'];
    if (url.startsWith('/products'))                return ['Catálogo', 'Productos'];
    return ['Catálogo'];
  }

  isActive(path: string): boolean {
    return this.router.url.startsWith(path);
  }

  logout(): void { this.authService.logout(); }
}
