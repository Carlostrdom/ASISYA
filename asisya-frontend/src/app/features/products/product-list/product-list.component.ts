import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ProductService, ProductDto, PagedResult } from '../../../core/services/product.service';
import { CategoryService, CategoryDto } from '../../../core/services/category.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-product-list',
  standalone: false,
  templateUrl: './product-list.component.html'
})
export class ProductListComponent implements OnInit {
  products: ProductDto[] = [];
  categories: CategoryDto[] = [];
  paged: Omit<PagedResult<ProductDto>, 'items'> = { totalCount: 0, page: 1, pageSize: 20, totalPages: 0 };

  search = '';
  categoryId: number | undefined;
  loading = false;

  sortKey: string = 'productName';
  sortDir = 1;
  viewMode: 'table' | 'grid' = 'table';

  confirmDelete: ProductDto | null = null;
  deleteLoading = false;

  bulkCount = 100000;
  bulkCategoryId = 1;
  bulkLoading = false;
  bulkMessage = '';
  showBulk = false;

  constructor(
    private productService: ProductService,
    private categoryService: CategoryService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadCategories();
    this.loadProducts();
  }

  loadCategories(): void {
    this.categoryService.getCategories().subscribe(cats => {
      this.categories = cats;
      if (cats.length > 0) this.bulkCategoryId = cats[0].categoryID;
    });
  }

  loadProducts(page = 1): void {
    this.loading = true;
    this.productService.getProducts(page, this.paged.pageSize, this.search || undefined, this.categoryId).subscribe({
      next: res => {
        this.products = res.items;
        this.paged = { totalCount: res.totalCount, page: res.page, pageSize: res.pageSize, totalPages: res.totalPages };
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  onSearch(): void { this.loadProducts(1); }
  onCategoryFilter(catId: number | undefined): void { this.categoryId = catId; this.loadProducts(1); }
  prevPage(): void { if (this.paged.page > 1) this.loadProducts(this.paged.page - 1); }
  nextPage(): void { if (this.paged.page < this.paged.totalPages) this.loadProducts(this.paged.page + 1); }

  edit(id: number): void { this.router.navigate(['/products/edit', id]); }
  create(): void { this.router.navigate(['/products/new']); }

  openDeleteModal(p: ProductDto): void { this.confirmDelete = p; }

  doDelete(): void {
    if (!this.confirmDelete) return;
    this.deleteLoading = true;
    const id = this.confirmDelete.productID;
    this.productService.deleteProduct(id).subscribe({
      next: () => {
        this.deleteLoading = false;
        this.confirmDelete = null;
        this.loadProducts(this.paged.page);
      },
      error: () => { this.deleteLoading = false; }
    });
  }

  toggleSort(key: string): void {
    if (this.sortKey === key) this.sortDir = -this.sortDir;
    else { this.sortKey = key; this.sortDir = 1; }
  }

  get sortedProducts(): ProductDto[] {
    return [...this.products].sort((a, b) => {
      const av = (a as any)[this.sortKey];
      const bv = (b as any)[this.sortKey];
      if (typeof av === 'number' && typeof bv === 'number') return (av - bv) * this.sortDir;
      return String(av ?? '').localeCompare(String(bv ?? '')) * this.sortDir;
    });
  }

  getSku(id: number): string {
    return `ASY-${id.toString().padStart(3, '0')}`;
  }

  getThumbStyle(id: number): string {
    const tones = [
      ['#cbc4b3', '#e3ddc9'], ['#a8b8a0', '#cad8c2'],
      ['#b8a692', '#d8c8b4'], ['#9aa6b8', '#c4cdd9'],
      ['#c9b09a', '#dec8b3'], ['#b5b0a6', '#d2cdc1'],
    ];
    const [c1, c2] = tones[id % 6];
    return `background: linear-gradient(135deg, ${c1}, ${c2})`;
  }

  getInitials(name: string): string {
    return name.split(' ').map(w => w[0]).join('').slice(0, 2).toUpperCase();
  }

  getStatus(p: ProductDto): { key: string; label: string } {
    if (p.discontinued) return { key: 'status-draft', label: 'Descontinuado' };
    if (p.unitsInStock === 0) return { key: 'status-out', label: 'Agotado' };
    if (p.unitsInStock < 10) return { key: 'status-low', label: 'Bajo stock' };
    return { key: 'status-active', label: 'En venta' };
  }

  fmtPrice(n: number): string {
    return new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'EUR' }).format(n);
  }

  get kpiActive(): number { return this.products.filter(p => !p.discontinued).length; }
  get kpiLow(): number { return this.products.filter(p => p.unitsInStock > 0 && p.unitsInStock < 10).length; }
  get kpiValue(): number { return this.products.reduce((s, p) => s + p.unitPrice * p.unitsInStock, 0); }

  bulkCreate(): void {
    this.bulkLoading = true;
    this.bulkMessage = '';
    this.productService.bulkCreate({ count: this.bulkCount, categoryID: this.bulkCategoryId }).subscribe({
      next: () => {
        this.bulkMessage = `✓ ${this.bulkCount} productos generados.`;
        this.bulkLoading = false;
        this.loadProducts(1);
      },
      error: () => { this.bulkMessage = 'Error al generar productos.'; this.bulkLoading = false; }
    });
  }

  logout(): void { this.authService.logout(); }

  get pages(): number[] {
    const total = this.paged.totalPages;
    const current = this.paged.page;
    const pages: number[] = [];
    for (let i = Math.max(1, current - 2); i <= Math.min(total, current + 2); i++) pages.push(i);
    return pages;
  }
}
