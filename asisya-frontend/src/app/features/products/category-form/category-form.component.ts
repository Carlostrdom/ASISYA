import { ChangeDetectorRef, Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { CategoryService } from '../../../core/services/category.service';

@Component({
  selector: 'app-category-form',
  standalone: false,
  templateUrl: './category-form.component.html'
})
export class CategoryFormComponent {
  form: FormGroup;
  loading = false;
  error = '';
  success = '';
  submitted = false;

  constructor(
    private fb: FormBuilder,
    private categoryService: CategoryService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {
    this.form = this.fb.group({
      categoryName: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', Validators.maxLength(500)]
    });
  }

  get categoryName() { return this.form.get('categoryName')!; }

  onSubmit(): void {
    this.submitted = true;
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading = true;
    this.error = '';
    this.success = '';

    this.categoryService.createCategory({
      categoryName: this.form.value.categoryName,
      description: this.form.value.description || null
    }).pipe(finalize(() => { this.loading = false; this.cdr.detectChanges(); })).subscribe({
      next: () => {
        this.success = 'Categoría creada correctamente.';
        this.cdr.detectChanges();
        setTimeout(() => this.router.navigate(['/products/categories']), 1200);
      },
      error: () => {
        this.error = 'Error al crear la categoría. Verifica los datos e intenta de nuevo.';
        this.cdr.detectChanges();
      }
    });
  }

  cancel(): void { this.router.navigate(['/products/categories']); }
}
