import { test, expect } from '@playwright/test';
import { registerNewUser } from './helpers/auth';

test.describe('Week Completion and Progression Flow', () => {
  test.beforeEach(async ({ page }) => {
    // Register a new user and create a workout
    await registerNewUser(page);
    
    // Create a workout with multiple exercises for testing progression
    await page.goto('/create-workout');
    await page.getByLabel(/workout.*name/i).fill('Progression Test Program');
    await page.getByLabel(/days.*week/i).selectOption('5');
    
    // Add first exercise (Squat)
    await page.getByRole('button', { name: /add.*exercise/i }).click();
    const exercises = page.locator('[data-testid="exercise-item"]');
    await exercises.nth(0).getByLabel(/exercise.*name/i).fill('Squat');
    await exercises.nth(0).getByLabel(/hevy.*template/i).fill('hevy-squat');
    await exercises.nth(0).getByLabel(/rest.*timer/i).fill('180');
    await exercises.nth(0).getByLabel(/training.*max/i).fill('140');
    await exercises.nth(0).getByLabel(/weight.*progression/i).fill('2.5');
    
    // Add second exercise (Bench Press)
    await page.getByRole('button', { name: /add.*exercise/i }).click();
    await exercises.nth(1).getByLabel(/exercise.*name/i).fill('Bench Press');
    await exercises.nth(1).getByLabel(/hevy.*template/i).fill('hevy-bench-press');
    await exercises.nth(1).getByLabel(/rest.*timer/i).fill('180');
    await exercises.nth(1).getByLabel(/training.*max/i).fill('100');
    await exercises.nth(1).getByLabel(/weight.*progression/i).fill('2.5');
    
    // Submit form
    await page.getByRole('button', { name: /create/i }).click();
    await expect(page).toHaveURL(/.*dashboard.*/);
  });

  test('should display current week information', async ({ page }) => {
    // Should show week and day
    await expect(page.getByText(/week.*1/i)).toBeVisible();
    await expect(page.getByText(/day.*1/i)).toBeVisible();
  });

  test('should advance through all days of the week', async ({ page }) => {
    // Complete 5 workout days in sequence
    for (let day = 1; day <= 5; day++) {
      // Verify current day
      await expect(page.getByText(new RegExp(`day.*${day}`, 'i'))).toBeVisible();

      // Start workout
      await page.getByRole('button', { name: /start.*workout/i }).click();

      // Complete all sets
      const setCheckboxes = page.locator('[data-testid="set-checkbox"]');
      const count = await setCheckboxes.count();

      for (let i = 0; i < count; i++) {
        await setCheckboxes.nth(i).click();
      }

      // Mark performance as success
      await page.getByLabel(/performance/i).selectOption('Success');

      // Complete workout
      await page.getByRole('button', { name: /complete.*workout/i }).click();

      // Should return to dashboard
      await expect(page).toHaveURL(/.*dashboard.*/);

      if (day < 5) {
        // Should advance to next day
        await expect(page.getByText(new RegExp(`day.*${day + 1}`, 'i'))).toBeVisible();
      }
    }

    // After completing day 5, should advance to week 2
    await expect(page.getByText(/week.*2/i)).toBeVisible();
    await expect(page.getByText(/day.*1/i)).toBeVisible();
  });

  test('should show week completion message', async ({ page }) => {
    // Navigate through days and complete them
    for (let day = 1; day <= 5; day++) {
      await page.getByRole('button', { name: /start.*workout/i }).click();

      const setCheckboxes = page.locator('[data-testid="set-checkbox"]');
      const count = await setCheckboxes.count();

      for (let i = 0; i < count; i++) {
        await setCheckboxes.nth(i).click();
      }

      await page.getByRole('button', { name: /complete.*workout/i }).click();
    }

    // After completing last day of week, should show completion message
    await expect(page.getByText(/week.*completed/i)).toBeVisible();
  });

  test('should apply progression to exercises', async ({ page }) => {
    // Get initial training max/weight
    await page.getByRole('button', { name: /start.*workout/i }).click();
    const initialWeight = await page.locator('[data-testid="exercise-weight"]').first().textContent();

    // Go back to dashboard
    await page.goBack();

    // Complete the workout successfully
    await page.getByRole('button', { name: /start.*workout/i }).click();

    const setCheckboxes = page.locator('[data-testid="set-checkbox"]');
    const count = await setCheckboxes.count();

    for (let i = 0; i < count; i++) {
      await setCheckboxes.nth(i).click();
    }

    await page.getByLabel(/performance/i).selectOption('Success');
    await page.getByRole('button', { name: /complete.*workout/i }).click();

    // Complete remaining days of week
    for (let day = 2; day <= 5; day++) {
      await page.getByRole('button', { name: /start.*workout/i }).click();

      const sets = page.locator('[data-testid="set-checkbox"]');
      const setCount = await sets.count();

      for (let i = 0; i < setCount; i++) {
        await sets.nth(i).click();
      }

      await page.getByRole('button', { name: /complete.*workout/i }).click();
    }

    // Start week 2, day 1
    await page.getByRole('button', { name: /start.*workout/i }).click();

    // Weight should have increased due to progression
    const newWeight = await page.locator('[data-testid="exercise-weight"]').first().textContent();

    // Weights should be different (progression applied)
    expect(initialWeight).not.toBe(newWeight);
  });

  test('should handle failed performance correctly', async ({ page }) => {
    // Start workout
    await page.getByRole('button', { name: /start.*workout/i }).click();

    // Get initial weight
    const initialWeight = await page.locator('[data-testid="exercise-weight"]').first().textContent();

    // Complete sets but mark as failed
    const setCheckboxes = page.locator('[data-testid="set-checkbox"]');
    const count = await setCheckboxes.count();

    for (let i = 0; i < count; i++) {
      await setCheckboxes.nth(i).click();
    }

    await page.getByLabel(/performance/i).selectOption('Failed');
    await page.getByRole('button', { name: /complete.*workout/i }).click();

    // Should show deload message
    await expect(page.getByText(/training.*max.*reduced/i)).toBeVisible();

    // Complete rest of week
    for (let day = 2; day <= 5; day++) {
      await page.getByRole('button', { name: /start.*workout/i }).click();

      const sets = page.locator('[data-testid="set-checkbox"]');
      const setCount = await sets.count();

      for (let i = 0; i < setCount; i++) {
        await sets.nth(i).click();
      }

      await page.getByRole('button', { name: /complete.*workout/i }).click();
    }

    // Check week 2 weight - should be reduced
    await page.getByRole('button', { name: /start.*workout/i }).click();
    const newWeight = await page.locator('[data-testid="exercise-weight"]').first().textContent();

    // Weight should have decreased due to failed performance
    expect(parseFloat(newWeight || '0')).toBeLessThan(parseFloat(initialWeight || '0'));
  });

  test('should display week progress tracking', async ({ page }) => {
    // Should show progress indicator
    await expect(page.getByText(/progress/i)).toBeVisible();

    // Should show completed days count
    await expect(page.getByText(/0.*\/.*5.*completed/i)).toBeVisible();

    // Complete one day
    await page.getByRole('button', { name: /start.*workout/i }).click();

    const setCheckboxes = page.locator('[data-testid="set-checkbox"]');
    const count = await setCheckboxes.count();

    for (let i = 0; i < count; i++) {
      await setCheckboxes.nth(i).click();
    }

    await page.getByRole('button', { name: /complete.*workout/i }).click();

    // Progress should update
    await expect(page.getByText(/1.*\/.*5.*completed/i)).toBeVisible();
  });

  test('should allow viewing workout history', async ({ page }) => {
    // Complete one workout
    await page.getByRole('button', { name: /start.*workout/i }).click();

    const setCheckboxes = page.locator('[data-testid="set-checkbox"]');
    const count = await setCheckboxes.count();

    for (let i = 0; i < count; i++) {
      await setCheckboxes.nth(i).click();
    }

    await page.getByRole('button', { name: /complete.*workout/i }).click();

    // Navigate to history
    await page.getByRole('link', { name: /history/i }).click();

    // Should show completed workout
    await expect(page.getByText(/week.*1.*day.*1/i)).toBeVisible();
    await expect(page.getByText(/completed/i)).toBeVisible();
  });
});
