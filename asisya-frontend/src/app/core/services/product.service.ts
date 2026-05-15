import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ProductDto {
  productID: number;
  productName: string;
  categoryID: number | null;
  categoryName: string | null;
  unitPrice: number;
  unitsInStock: number;
  discontinued: boolean;
}

export interface ProductDetailDto extends ProductDto {
  supplierID: number | null;
  supplierName: string | null;
  quantityPerUnit: string | null;
  unitsOnOrder: number;
  reorderLevel: number;
  categoryPictureBase64: string | null;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateProductDto {
  productName: string;
  categoryID: number | null;
  supplierID: number | null;
  quantityPerUnit: string | null;
  unitPrice: number;
  unitsInStock: number;
  unitsOnOrder: number;
  reorderLevel: number;
  discontinued: boolean;
}

export interface BulkCreateDto { count: number; categoryID: number; }

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly url = `${environment.apiUrl}/product`;

  constructor(private http: HttpClient) {}

  getProducts(page = 1, pageSize = 20, search?: string, categoryId?: number): Observable<PagedResult<ProductDto>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search) params = params.set('search', search);
    if (categoryId) params = params.set('categoryId', categoryId);
    return this.http.get<PagedResult<ProductDto>>(this.url, { params });
  }

  getProduct(id: number): Observable<ProductDetailDto> {
    return this.http.get<ProductDetailDto>(`${this.url}/${id}`);
  }

  createProduct(dto: CreateProductDto): Observable<ProductDto> {
    return this.http.post<ProductDto>(this.url, dto);
  }

  bulkCreate(dto: BulkCreateDto): Observable<any> {
    return this.http.post(`${this.url}/bulk`, dto);
  }

  updateProduct(id: number, dto: Partial<CreateProductDto>): Observable<void> {
    return this.http.put<void>(`${this.url}/${id}`, dto);
  }

  deleteProduct(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
