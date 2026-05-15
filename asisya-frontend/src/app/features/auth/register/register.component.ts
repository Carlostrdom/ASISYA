import { Component } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: false,
  templateUrl: './register.component.html'
})
export class RegisterComponent {
  form: FormGroup;
  error = '';
  success = '';
  loading = false;

  constructor(private fb: FormBuilder, private authService: AuthService, private router: Router) {
    this.form = this.fb.group({
      username: ['', [Validators.required, Validators.minLength(3)]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirm:  ['', [Validators.required]]
    }, { validators: this.passwordsMatch });
  }

  passwordsMatch(g: AbstractControl) {
    return g.get('password')?.value === g.get('confirm')?.value ? null : { mismatch: true };
  }

  get username() { return this.form.get('username')!; }
  get password() { return this.form.get('password')!; }
  get confirm()  { return this.form.get('confirm')!; }

  onSubmit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading = true;
    this.error = '';
    this.authService.register({ username: this.form.value.username, password: this.form.value.password }).subscribe({
      next: () => {
        this.loading = false;
        this.success = 'Cuenta creada. Redirigiendo al login…';
        setTimeout(() => this.router.navigate(['/login']), 1400);
      },
      error: (err) => {
        this.error = err?.error?.message || 'Error al crear la cuenta. El usuario puede ya existir.';
        this.loading = false;
      }
    });
  }
}
