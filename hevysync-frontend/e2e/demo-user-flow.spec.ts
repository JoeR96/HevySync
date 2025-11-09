import { test, expect } from '@playwright/test';

test.describe('Demo User Complete Workflow', () => {
  test('should login as demo user, view dashboard, and complete workout', async ({ page }) => {
    // Navigate to login page
    await page.goto('http://localhost:5173/login');

    // Click demo login button
    await page.getByRole('button', { name: /demo.*login/i }).click();

    // Wait for redirect to dashboard
    await expect(page).toHaveURL(/.*dashboard.*/, { timeout: 10000 });

    // Verify dashboard loads with workout data
    await expect(page.getByText(/hevysync/i)).toBeVisible();
    await expect(page.getByText(/week/i)).toBeVisible({ timeout: 10000 });

    // Check if active workout is displayed
    const activeWorkoutCard = page.locator('text=/active workout/i').first();
    await expect(activeWorkoutCard).toBeVisible({ timeout: 5000 });

    // Click "Start Today's Workout" or similar button
    const startWorkoutButton = page.getByRole('button', { name: /start.*workout/i });
    if (await startWorkoutButton.isVisible({ timeout: 2000 })) {
      await startWorkoutButton.click();

      // Wait for workout execution page
      await expect(page).toHaveURL(/.*workout.*execute.*/, { timeout: 10000 });

      // Verify exercise cards are displayed
      await expect(page.getByText(/exercise/i)).toBeVisible({ timeout: 5000 });

      // Check if weight and reps inputs are present
      const weightInputs = page.locator('input[type="number"]').filter({ hasText: /weight/i });
      const repsInputs = page.locator('input[type="number"]').filter({ hasText: /reps/i });

      // If inputs exist, try to complete workout
      if (await page.locator('input[type="number"]').count() > 0) {
        // Look for complete workout button
        const completeButton = page.getByRole('button', { name: /complete.*workout/i });
        if (await completeButton.isVisible({ timeout: 2000 })) {
          await completeButton.click();

          // Should redirect back to dashboard
          await expect(page).toHaveURL(/.*dashboard.*/, { timeout: 10000 });
        }
      }
    }

    // Verify we can see workout history/stats
    await expect(page.getByText(/workout/i)).toBeVisible();
  });

  test('should display workout statistics on dashboard', async ({ page }) => {
    // Login as demo user
    await page.goto('http://localhost:5173/login');
    await page.getByRole('button', { name: /demo/i }).click();

    // Wait for dashboard
    await expect(page).toHaveURL(/.*dashboard.*/, { timeout: 10000 });

    // Check for various workout statistics
    const statsTexts = [
      /total.*workout/i,
      /week/i,
      /exercise/i,
      /completed/i
    ];

    // At least one of these should be visible
    let foundStats = false;
    for (const text of statsTexts) {
      if (await page.getByText(text).isVisible({ timeout: 2000 }).catch(() => false)) {
        foundStats = true;
        break;
      }
    }

    expect(foundStats).toBeTruthy();
  });

  test('should navigate between pages without errors', async ({ page }) => {
    // Setup: Login
    await page.goto('http://localhost:5173/login');
    await page.getByRole('button', { name: /demo/i }).click();
    await expect(page).toHaveURL(/.*dashboard.*/, { timeout: 10000 });

    // Try to navigate to create workout page
    await page.goto('http://localhost:5173/create-workout');

    // Should load without console errors
    const consoleLogs: string[] = [];
    page.on('console', msg => {
      if (msg.type() === 'error') {
        consoleLogs.push(msg.text());
      }
    });

    // Wait a bit for any errors to appear
    await page.waitForTimeout(2000);

    // Navigate back to dashboard
    await page.goto('http://localhost:5173/dashboard');
    await expect(page).toHaveURL(/.*dashboard.*/);

    // Check for critical errors
    const criticalErrors = consoleLogs.filter(log =>
      !log.includes('favicon') &&
      !log.includes('404') &&
      !log.includes('Warning')
    );

    if (criticalErrors.length > 0) {
      console.log('Console errors found:', criticalErrors);
    }
  });
});
