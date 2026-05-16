import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { ProductService } from '../../../core/services/product.service';
import { CategoryService, CategoryDto } from '../../../core/services/category.service';

@Component({
  selector: 'app-product-form',
  standalone: false,
  templateUrl: './product-form.component.html'
})
export class ProductFormComponent implements OnInit {
  form: FormGroup;
  categories: CategoryDto[] = [];
  isEdit = false;
  productId: number | null = null;
  loading = false;
  error = '';
  success = '';
  submitted = false;

  constructor(
    private fb: FormBuilder,
    private productService: ProductService,
    private categoryService: CategoryService,
    private route: ActivatedRoute,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {
    this.form = this.fb.group({
      productName: ['', [Validators.required, Validators.maxLength(100)]],
      categoryID: [null],
      supplierID: [null],
      quantityPerUnit: [''],
      unitPrice: [0, [Validators.required, Validators.min(0)]],
      unitsInStock: [0, [Validators.required, Validators.min(0)]],
      unitsOnOrder: [0, [Validators.min(0)]],
      reorderLevel: [0, [Validators.min(0)]],
      discontinued: [false]
    });
  }

  ngOnInit(): void {
    this.categoryService.getCategories().subscribe(cats => { this.categories = cats; this.cdr.detectChanges(); });
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.productId = +id;
      this.productService.getProduct(this.productId).subscribe(p => {
        this.form.patchValue({
          productName: p.productName,
          categoryID: p.categoryID,
          supplierID: p.supplierID,
          quantityPerUnit: p.quantityPerUnit,
          unitPrice: p.unitPrice,
          unitsInStock: p.unitsInStock,
          unitsOnOrder: p.unitsOnOrder,
          reorderLevel: p.reorderLevel,
          discontinued: p.discontinued
        });
        this.cdr.detectChanges();
      });
    }
  }

  get productName() { return this.form.get('productName')!; }
  get unitPrice() { return this.form.get('unitPrice')!; }
  get unitsInStock() { return this.form.get('unitsInStock')!; }

  get touchedCount(): number {
    return Object.values(this.form.controls).filter(c => c.touched).length;
  }

  get totalControls(): number {
    return Object.keys(this.form.controls).length;
  }

  fmtPrice(n: number): string {
    return new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'EUR' }).format(n);
  }

  onSubmit(): void {
    this.submitted = true;
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading = true;
    this.error = '';
    this.success = '';
    const value = this.form.value;

    const request$: Observable<unknown> = this.isEdit
      ? this.productService.updateProduct(this.productId!, value)
      : this.productService.createProduct(value);

    request$.pipe(finalize(() => { this.loading = false; this.cdr.detectChanges(); })).subscribe({
      next: () => {
        this.success = this.isEdit ? 'Producto actualizado correctamente.' : 'Producto creado correctamente.';
        this.cdr.detectChanges();
        setTimeout(() => this.router.navigate(['/products']), 1200);
      },
      error: () => {
        this.error = 'Error al guardar el producto. Verifica los datos e intenta de nuevo.';
        this.cdr.detectChanges();
      }
    });
  }

  cancel(): void { this.router.navigate(['/products']); }
}
