import { test, expect } from '@playwright/test';

test('verify demo login and workout data loads', async ({ page }) => {
  // Navigate to login page
  await page.goto('http://localhost:5173/login');

  // Wait for page to be fully loaded
  await page.waitForLoadState('networkidle');

  // Click the demo login button
  await page.click('button:has-text("Demo Login (Pre-loaded Data)")');

  // Wait for redirect to dashboard
  await page.waitForURL('**/dashboard', { timeout: 10000 });

  // Wait for workout data to load
  await page.waitForTimeout(2000);

  // Check that workouts are visible on the dashboard
  const workoutElements = await page.locator('text=/A2S Hypertrophy/').count();
  console.log(`Found ${workoutElements} workout elements`);

  expect(workoutElements).toBeGreaterThanOrEqual(1);

  // Take a screenshot of the dashboard
  await page.screenshot({ path: 'dashboard-loaded.png', fullPage: true });

  console.log('✅ Login successful and dashboard loaded with workout data');
});
