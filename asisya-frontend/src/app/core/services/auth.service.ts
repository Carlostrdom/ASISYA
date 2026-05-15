import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface LoginDto { username: string; password: string; }
export interface RegisterDto { username: string; password: string; }
export interface TokenDto { token: string; expiresAt: string; }

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'asisya_token';
  private readonly url = `${environment.apiUrl}/auth`;

  constructor(private http: HttpClient, private router: Router) {}

  login(dto: LoginDto): Observable<TokenDto> {
    return this.http.post<TokenDto>(`${this.url}/login`, dto).pipe(
      tap(res => localStorage.setItem(this.tokenKey, res.token))
    );
  }

  register(dto: RegisterDto): Observable<any> {
    return this.http.post(`${this.url}/register`, dto);
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }
}
