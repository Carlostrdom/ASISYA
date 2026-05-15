import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ProductsRoutingModule } from './products-routing-module';
import { ProductListComponent } from './product-list/product-list.component';
import { ProductFormComponent } from './product-form/product-form.component';
import { ShellComponent } from './shell/shell.component';

@NgModule({
  declarations: [ProductListComponent, ProductFormComponent, ShellComponent],
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterModule, ProductsRoutingModule]
})
export class ProductsModule { }
