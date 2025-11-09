import { spawn } from 'child_process';
import { fileURLToPath } from 'url';
import { dirname, join } from 'path';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

// Start backend
const backend = spawn('dotnet', ['run', '--project', '../HevySync/HevySync.csproj'], {
  cwd: __dirname,
  shell: true,
  stdio: 'inherit'
});

// Start frontend
const frontend = spawn('npm', ['run', 'dev'], {
  cwd: __dirname,
  shell: true,
  stdio: 'inherit'
});

// Handle cleanup
process.on('SIGINT', () => {
  backend.kill();
  frontend.kill();
  process.exit();
});

process.on('SIGTERM', () => {
  backend.kill();
  frontend.kill();
  process.exit();
});
