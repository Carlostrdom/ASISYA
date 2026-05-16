import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CategoryService, CategoryDto } from '../../../core/services/category.service';

@Component({
  selector: 'app-category-list',
  standalone: false,
  templateUrl: './category-list.component.html'
})
export class CategoryListComponent implements OnInit {
  categories: CategoryDto[] = [];
  loading = false;
  deleteTarget: CategoryDto | null = null;

  constructor(private categoryService: CategoryService, private router: Router, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.categoryService.getCategories().subscribe({
      next: cats => { this.categories = cats; this.loading = false; this.cdr.detectChanges(); },
      error: () => { this.loading = false; this.cdr.detectChanges(); }
    });
  }

  create(): void { this.router.navigate(['/products/categories/new']); }
  back(): void { this.router.navigate(['/products']); }
}
