import { Box, CircularProgress, Typography } from '@mui/material';

interface LoadingSpinnerProps {
  message?: string;
  size?: number;
  fullScreen?: boolean;
}

export function LoadingSpinner({ message = 'Loading...', size = 40, fullScreen = false }: LoadingSpinnerProps) {
  const spinnerContent = (
    <Box sx={{ 
      display: 'flex', 
      flexDirection: 'column',
      alignItems: 'center', 
      justifyContent: 'center', 
      gap: 2
    }}>
      <CircularProgress size={size} />
      <Typography variant="body1" color="text.secondary">
        {message}
      </Typography>
    </Box>
  );

  if (fullScreen) {
    return (
      <Box sx={{ 
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        bgcolor: 'background.paper',
        zIndex: 9999
      }}>
        {spinnerContent}
      </Box>
    );
  }

  return (
    <Box sx={{ 
      display: 'flex', 
      alignItems: 'center', 
      justifyContent: 'center', 
      py: 8
    }}>
      {spinnerContent}
    </Box>
  );
} 