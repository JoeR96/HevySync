import { test, expect } from '@playwright/test';

test('test dashboard with console logging', async ({ page }) => {
  const consoleMessages: string[] = [];
  const errors: string[] = [];

  page.on('console', msg => {
    consoleMessages.push(`${msg.type()}: ${msg.text()}`);
    if (msg.type() === 'error') {
      errors.push(msg.text());
    }
  });

  page.on('pageerror', error => {
    errors.push(`Page error: ${error.message}`);
  });

  page.on('response', async response => {
    if (response.url().includes('/average2savage/workouts')) {
      console.log(`\n=== API Response for /workouts ===`);
      console.log(`Status: ${response.status()}`);
      if (response.status() === 200) {
        try {
          const body = await response.json();
          console.log(`Body: ${JSON.stringify(body, null, 2)}`);
        } catch (e) {
          console.log(`Could not parse JSON: ${e}`);
        }
      } else {
        const text = await response.text();
        console.log(`Error response: ${text}`);
      }
    }
  });

  // Navigate to login page
  await page.goto('http://localhost:5173/login');
  await page.waitForLoadState('networkidle');

  // Click demo login
  await page.click('button:has-text("Demo Login (Pre-loaded Data)")');

  // Wait for dashboard
  await page.waitForURL('**/dashboard', { timeout: 10000 });

  // Wait for data to load
  await page.waitForTimeout(3000);

  // Take screenshot
  await page.screenshot({ path: 'dashboard-test.png', fullPage: true });

  // Log all HTML
  const html = await page.content();
  console.log('\n=== Page HTML ===');
  console.log(html.substring(0, 2000));

  console.log('\n=== All Console Messages ===');
  consoleMessages.forEach(msg => console.log(msg));

  if (errors.length > 0) {
    console.log('\n=== Errors ===');
    errors.forEach(error => console.log(error));
  }
});
