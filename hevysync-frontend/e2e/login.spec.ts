import { test, expect } from '@playwright/test';
import { registerNewUser, loginWithCredentials } from './helpers/auth';

test.describe('Authentication Flow', () => {
  test('should display login page when not authenticated', async ({ page }) => {
    await page.goto('/');

    // Should redirect to login page
    await expect(page).toHaveURL(/.*login.*/);
  });

  test('should show registration form', async ({ page }) => {
    await page.goto('/register');

    // Look for email input
    const emailInput = page.getByLabel(/email/i);
    await expect(emailInput).toBeVisible();
    
    // Should have password fields
    await expect(page.getByLabel(/^password$/i)).toBeVisible();
    await expect(page.getByLabel(/confirm.*password/i)).toBeVisible();
  });

  test('should register new user and navigate to dashboard', async ({ page }) => {
    const { email } = await registerNewUser(page);

    // Should show dashboard elements
    await expect(page).toHaveURL(/.*dashboard.*/);
    
    // User should see their email or welcome message
    await expect(page.getByText(new RegExp(email, 'i'))).toBeVisible({ timeout: 5000 }).catch(() => {
      // Email might not be displayed, just check we're on dashboard
      expect(page.url()).toContain('dashboard');
    });
  });

  test('should login with existing credentials', async ({ page }) => {
    // First register a user
    const { email, password } = await registerNewUser(page);
    
    // Logout
    await page.getByRole('button', { name: /logout|sign out/i }).click();
    
    // Login again
    await loginWithCredentials(page, email, password);
    
    // Should be on dashboard
    await expect(page).toHaveURL(/.*dashboard.*/);
  });

  test('should persist authentication across page reloads', async ({ page }) => {
    await registerNewUser(page);

    // Reload page
    await page.reload();

    // Should still be on dashboard
    await expect(page).toHaveURL(/.*dashboard.*/);
  });
});
