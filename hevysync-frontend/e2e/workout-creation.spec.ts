import { test, expect } from '@playwright/test';
import { registerNewUser } from './helpers/auth';

test.describe('Workout Creation Flow', () => {
  test.beforeEach(async ({ page }) => {
    // Register a new user before each test
    await registerNewUser(page);
  });

  test('should navigate to create workout page', async ({ page }) => {
    // Click create workout button
    const createButton = page.getByRole('button', { name: /create.*workout/i });
    await createButton.click();

    // Should navigate to create workout page
    await expect(page).toHaveURL(/.*create-workout.*/);
    await expect(page.getByText(/create.*workout/i)).toBeVisible();
  });

  test('should display workout creation form', async ({ page }) => {
    await page.goto('/create-workout');

    // Should show form fields
    await expect(page.getByLabel(/workout.*name/i)).toBeVisible();
    await expect(page.getByLabel(/days.*week/i)).toBeVisible();
  });

  test('should create workout with valid data', async ({ page }) => {
    await page.goto('/create-workout');

    // Fill in workout name
    await page.getByLabel(/workout.*name/i).fill('Test Average 2 Savage Program');

    // Select days in week
    await page.getByLabel(/days.*week/i).selectOption('5');

    // Add first exercise (Squat)
    await page.getByRole('button', { name: /add.*exercise/i }).click();
    await page.getByLabel(/exercise.*name/i).first().fill('Squat');
    await page.getByLabel(/hevy.*template/i).first().fill('hevy-squat');
    await page.getByLabel(/rest.*timer/i).first().fill('180');

    // Configure linear progression
    await page.getByLabel(/training.*max/i).first().fill('140');
    await page.getByLabel(/weight.*progression/i).first().fill('2.5');

    // Submit form
    await page.getByRole('button', { name: /create/i }).click();

    // Should redirect to dashboard
    await expect(page).toHaveURL(/.*dashboard.*/);

    // Should show success message
    await expect(page.getByText(/workout.*created/i)).toBeVisible();

    // Should display the new workout
    await expect(page.getByText(/Test Average 2 Savage Program/i)).toBeVisible();
  });

  test('should validate required fields', async ({ page }) => {
    await page.goto('/create-workout');

    // Try to submit without filling form
    await page.getByRole('button', { name: /create/i }).click();

    // Should show validation errors
    await expect(page.getByText(/required/i)).toBeVisible();
  });

  test('should create workout with multiple exercises', async ({ page }) => {
    await page.goto('/create-workout');

    // Fill in workout name
    await page.getByLabel(/workout.*name/i).fill('Multi-Exercise Program');
    await page.getByLabel(/days.*week/i).selectOption('5');

    // Add first exercise (Squat with linear progression)
    await page.getByRole('button', { name: /add.*exercise/i }).click();
    const exercises = page.locator('[data-testid="exercise-item"]');
    await exercises.nth(0).getByLabel(/exercise.*name/i).fill('Squat');
    await exercises.nth(0).getByLabel(/hevy.*template/i).fill('hevy-squat');
    await exercises.nth(0).getByLabel(/rest.*timer/i).fill('180');
    await exercises.nth(0).getByLabel(/training.*max/i).fill('140');

    // Add second exercise (RDL with reps per set)
    await page.getByRole('button', { name: /add.*exercise/i }).click();
    await exercises.nth(1).getByLabel(/exercise.*name/i).fill('Romanian Deadlift');
    await exercises.nth(1).getByLabel(/hevy.*template/i).fill('hevy-rdl');
    await exercises.nth(1).getByLabel(/rest.*timer/i).fill('120');

    // Switch to reps per set strategy
    await exercises.nth(1).getByLabel(/progression.*type/i).selectOption('reps-per-set');
    await exercises.nth(1).getByLabel(/starting.*weight/i).fill('80');
    await exercises.nth(1).getByLabel(/min.*reps/i).fill('8');
    await exercises.nth(1).getByLabel(/target.*reps/i).fill('10');
    await exercises.nth(1).getByLabel(/max.*reps/i).fill('12');

    // Submit form
    await page.getByRole('button', { name: /create/i }).click();

    // Should redirect to dashboard
    await expect(page).toHaveURL(/.*dashboard.*/);
    await expect(page.getByText(/workout.*created/i)).toBeVisible();
  });

  test('should allow removing exercises', async ({ page }) => {
    await page.goto('/create-workout');

    // Add two exercises
    await page.getByRole('button', { name: /add.*exercise/i }).click();
    await page.getByRole('button', { name: /add.*exercise/i }).click();

    const exercises = page.locator('[data-testid="exercise-item"]');
    await expect(exercises).toHaveCount(2);

    // Remove first exercise
    await exercises.first().getByRole('button', { name: /remove/i }).click();

    // Should only have one exercise left
    await expect(exercises).toHaveCount(1);
  });
});
