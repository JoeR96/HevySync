import { Page, expect } from '@playwright/test';

let testUserCounter = 0;

export async function registerNewUser(page: Page) {
  // Generate unique email for this test user
  const timestamp = Date.now();
  const counter = ++testUserCounter;
  const email = `test-user-${timestamp}-${counter}@hevysync.com`;
  const password = 'TestPassword123!';

  await page.goto('/register');

  // Wait for page to load
  await page.waitForLoadState('networkidle');

  // Fill in registration form
  await page.getByLabel(/email/i).fill(email);
  await page.getByLabel(/^password$/i).fill(password);
  await page.getByLabel(/confirm.*password/i).fill(password);

  // Click register button and wait for navigation
  await Promise.all([
    page.waitForURL(/.*dashboard.*|.*login.*/, { timeout: 15000 }),
    page.getByRole('button', { name: /create account/i }).click()
  ]);

  // If redirected to login, log in with the new credentials
  if (page.url().includes('login')) {
    await page.getByLabel(/email/i).fill(email);
    await page.getByLabel(/password/i).fill(password);
    await Promise.all([
      page.waitForURL(/.*dashboard.*/, { timeout: 15000 }),
      page.getByRole('button', { name: /continue|sign in|login/i }).click()
    ]);
  }

  return { email, password };
}

export async function loginWithCredentials(page: Page, email: string, password: string) {
  await page.goto('/login');

  await page.getByLabel(/email/i).fill(email);
  await page.getByLabel(/password/i).fill(password);

  await page.getByRole('button', { name: /continue|sign in|login/i }).click();

  await expect(page).toHaveURL(/.*dashboard.*/, { timeout: 10000 });
}
