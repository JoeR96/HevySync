import { test, expect } from '@playwright/test';
import { registerNewUser } from './helpers/auth';

test.describe('Workout Day Completion Flow', () => {
  test.beforeEach(async ({ page }) => {
    // Register a new user and create a workout
    await registerNewUser(page);
    
    // Create a workout with exercises
    await page.goto('/create-workout');
    await page.getByLabel(/workout.*name/i).fill('Test Workout Program');
    await page.getByLabel(/days.*week/i).selectOption('5');
    
    // Add first exercise
    await page.getByRole('button', { name: /add.*exercise/i }).click();
    await page.getByLabel(/exercise.*name/i).first().fill('Squat');
    await page.getByLabel(/hevy.*template/i).first().fill('hevy-squat');
    await page.getByLabel(/rest.*timer/i).first().fill('180');
    await page.getByLabel(/training.*max/i).first().fill('140');
    await page.getByLabel(/weight.*progression/i).first().fill('2.5');
    
    // Submit form
    await page.getByRole('button', { name: /create/i }).click();
    await expect(page).toHaveURL(/.*dashboard.*/);
  });

  test('should display current workout day', async ({ page }) => {
    // Should show current workout information
    await expect(page.getByText(/week.*1/i)).toBeVisible();
    await expect(page.getByText(/day.*1/i)).toBeVisible();

    // Should show today's exercises
    await expect(page.getByText(/exercises.*today/i)).toBeVisible();
  });

  test('should show exercise details for current day', async ({ page }) => {
    // Click on current workout
    await page.getByRole('button', { name: /start.*workout/i }).click();

    // Should navigate to workout page
    await expect(page).toHaveURL(/.*workout.*/);

    // Should display exercises with sets, reps, and weights
    await expect(page.locator('[data-testid="exercise-card"]')).toHaveCount.greaterThan(0);

    // Should show set information
    await expect(page.getByText(/set.*1/i)).toBeVisible();
    await expect(page.getByText(/reps/i)).toBeVisible();
    await expect(page.getByText(/weight/i)).toBeVisible();
  });

  test('should allow marking sets as complete', async ({ page }) => {
    await page.getByRole('button', { name: /start.*workout/i }).click();

    // Find first set checkbox/button
    const firstSet = page.locator('[data-testid="set-checkbox"]').first();
    await firstSet.click();

    // Should be marked as complete (checked or highlighted)
    await expect(firstSet).toBeChecked();
  });

  test('should complete workout day successfully', async ({ page }) => {
    await page.getByRole('button', { name: /start.*workout/i }).click();

    // Complete all sets for all exercises
    const setCheckboxes = page.locator('[data-testid="set-checkbox"]');
    const count = await setCheckboxes.count();

    for (let i = 0; i < count; i++) {
      await setCheckboxes.nth(i).click();
    }

    // Click complete workout button
    await page.getByRole('button', { name: /complete.*workout/i }).click();

    // Should show success message
    await expect(page.getByText(/workout.*completed/i)).toBeVisible();

    // Should navigate back to dashboard
    await expect(page).toHaveURL(/.*dashboard.*/);

    // Should show next day
    await expect(page.getByText(/day.*2/i)).toBeVisible();
  });

  test('should track performance results', async ({ page }) => {
    await page.getByRole('button', { name: /start.*workout/i }).click();

    // Complete all sets
    const setCheckboxes = page.locator('[data-testid="set-checkbox"]');
    const count = await setCheckboxes.count();

    for (let i = 0; i < count; i++) {
      await setCheckboxes.nth(i).click();
    }

    // Select performance result (Success/Failed/Partial)
    await page.getByLabel(/performance/i).selectOption('Success');

    // Complete workout
    await page.getByRole('button', { name: /complete.*workout/i }).click();

    // Should show progression applied message
    await expect(page.getByText(/progression.*applied/i)).toBeVisible();
  });

  test('should display rest timer between sets', async ({ page }) => {
    await page.getByRole('button', { name: /start.*workout/i }).click();

    // Complete first set
    await page.locator('[data-testid="set-checkbox"]').first().click();

    // Should show rest timer
    await expect(page.getByText(/rest/i)).toBeVisible();
    await expect(page.getByText(/\d+:\d+/)).toBeVisible(); // MM:SS format
  });

  test('should allow editing set performance', async ({ page }) => {
    await page.getByRole('button', { name: /start.*workout/i }).click();

    // Find first set
    const firstSet = page.locator('[data-testid="exercise-set"]').first();

    // Edit weight
    const weightInput = firstSet.getByLabel(/weight/i);
    await weightInput.clear();
    await weightInput.fill('105');

    // Edit reps
    const repsInput = firstSet.getByLabel(/reps/i);
    await repsInput.clear();
    await repsInput.fill('6');

    // Values should be updated
    await expect(weightInput).toHaveValue('105');
    await expect(repsInput).toHaveValue('6');
  });

  test('should prevent completing workout with incomplete sets', async ({ page }) => {
    await page.getByRole('button', { name: /start.*workout/i }).click();

    // Try to complete without marking all sets
    await page.getByRole('button', { name: /complete.*workout/i }).click();

    // Should show validation error
    await expect(page.getByText(/complete.*all.*sets/i)).toBeVisible();
  });
});
