import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CategoryDto {
  categoryID: number;
  categoryName: string;
  description: string | null;
}

export interface CreateCategoryDto {
  categoryName: string;
  description: string | null;
}

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private readonly url = `${environment.apiUrl}/category`;

  constructor(private http: HttpClient) {}

  getCategories(): Observable<CategoryDto[]> {
    return this.http.get<CategoryDto[]>(this.url);
  }

  createCategory(dto: CreateCategoryDto): Observable<CategoryDto> {
    return this.http.post<CategoryDto>(this.url, dto);
  }
}
