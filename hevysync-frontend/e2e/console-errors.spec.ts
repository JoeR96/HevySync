import { test, expect } from '@playwright/test';

test('capture console errors on login page', async ({ page }) => {
  const errors: string[] = [];

  page.on('console', msg => {
    if (msg.type() === 'error') {
      errors.push(msg.text());
    }
  });

  page.on('pageerror', error => {
    errors.push(`Page error: ${error.message}`);
  });

  await page.goto('http://localhost:5173/login');

  // Wait a bit for errors to appear
  await page.waitForTimeout(3000);

  console.log('=== CONSOLE ERRORS ===');
  errors.forEach(error => console.log(error));
  console.log('=== END ERRORS ===');

  // Take a screenshot
  await page.screenshot({ path: 'login-error.png', fullPage: true });
});
