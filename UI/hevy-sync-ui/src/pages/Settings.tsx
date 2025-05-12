import { useState } from 'react';
import { 
  Box, 
  Container, 
  TextField, 
  Button, 
  Typography, 
  Paper,
  Switch,
  FormControlLabel,
  Alert
} from '@mui/material';
import { apiClient } from '../lib/api';
import { useThemeStore } from '../stores/theme';
import { LoadingSpinner } from '../components/LoadingSpinner';

export function Settings() {
  const [hevyApiKey, setHevyApiKey] = useState('');
  const [isSettingKey, setIsSettingKey] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const { isDarkMode, toggleDarkMode } = useThemeStore();

  const handleSetApiKey = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSettingKey(true);
    setError(null);
    setSuccess(null);
    try {
      await apiClient.setHevyApiKey(hevyApiKey);
      setHevyApiKey('');
      setSuccess('API key set successfully!');
    } catch (err) {
      setError('Failed to set API key. Please try again.');
    } finally {
      setIsSettingKey(false);
    }
  };

  return (
    <Container maxWidth="md" sx={{ py: 4 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Settings
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      {success && (
        <Alert severity="success" sx={{ mb: 2 }}>
          {success}
        </Alert>
      )}

      <Paper sx={{ p: 3, mb: 4 }}>
        <Typography variant="h6" gutterBottom>
          Appearance
        </Typography>
        <FormControlLabel
          control={
            <Switch
              checked={isDarkMode}
              onChange={toggleDarkMode}
              color="primary"
            />
          }
          label="Dark Mode"
        />
      </Paper>

      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Hevy API Key
        </Typography>
        <form onSubmit={handleSetApiKey}>
          <Box sx={{ display: 'flex', gap: 2 }}>
            <TextField
              fullWidth
              label="Hevy API Key"
              value={hevyApiKey}
              onChange={(e) => setHevyApiKey(e.target.value)}
              type="password"
              required
            />
            <Button
              type="submit"
              variant="contained"
              disabled={isSettingKey}
              sx={{ minWidth: 120 }}
            >
              {isSettingKey ? (
                <LoadingSpinner size={24} message="Setting..." />
              ) : (
                'Set Key'
              )}
            </Button>
          </Box>
        </form>
      </Paper>
    </Container>
  );
} 