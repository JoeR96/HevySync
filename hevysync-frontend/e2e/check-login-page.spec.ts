import { test, expect } from '@playwright/test';

test('check login page loads', async ({ page }) => {
  // Navigate to login page
  await page.goto('http://localhost:5173/login');

  // Wait for page to be fully loaded
  await page.waitForLoadState('networkidle');

  // Take a screenshot
  await page.screenshot({ path: 'login-page.png', fullPage: true });

  // Get all button text
  const buttons = await page.locator('button').all();
  console.log(`Found ${buttons.length} buttons`);
  for (const button of buttons) {
    const text = await button.textContent();
    console.log(`Button text: "${text}"`);
  }

  // Check if there are any console errors
  const errors: string[] = [];
  page.on('console', msg => {
    if (msg.type() === 'error') {
      errors.push(msg.text());
    }
  });

  page.on('pageerror', error => {
    errors.push(`Page error: ${error.message}`);
  });

  await page.waitForTimeout(2000);

  if (errors.length > 0) {
    console.log('=== CONSOLE ERRORS ===');
    errors.forEach(error => console.log(error));
    console.log('=== END ERRORS ===');
  } else {
    console.log('✅ No console errors');
  }
});
