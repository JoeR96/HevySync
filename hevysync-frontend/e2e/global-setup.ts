import { chromium, FullConfig } from '@playwright/test';

const API_URL = process.env.VITE_API_URL || 'http://localhost:5189';
const MAX_RETRIES = 30;
const RETRY_DELAY = 1000;

async function waitForBackend() {
  console.log(`Waiting for backend at ${API_URL}...`);
  
  for (let i = 0; i < MAX_RETRIES; i++) {
    try {
      const response = await fetch(`${API_URL}/health`, { method: 'GET' });
      if (response.ok || response.status === 404) {
        // 404 is ok - means server is up but no health endpoint
        console.log('✅ Backend is ready!');
        return;
      }
    } catch (error) {
      // Server not ready yet
    }
    
    if (i < MAX_RETRIES - 1) {
      await new Promise(resolve => setTimeout(resolve, RETRY_DELAY));
    }
  }
  
  throw new Error(`Backend at ${API_URL} is not responding. Please start the backend with: dotnet run --project ../HevySync/HevySync.csproj`);
}

async function globalSetup(config: FullConfig) {
  await waitForBackend();
}

export default globalSetup;
