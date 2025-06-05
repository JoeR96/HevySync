import { Box, Paper, Typography } from '@mui/material';
import { useNavigate } from 'react-router-dom';

interface NavButtonProps {
  title: string;
  icon: React.ReactNode;
  path: string;
  description: string;
}

export function NavButton({ title, icon, path, description }: NavButtonProps) {
  const navigate = useNavigate();

  return (
    <Paper
      sx={{
        p: 3,
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        textAlign: 'center',
        cursor: 'pointer',
        transition: 'transform 0.2s, box-shadow 0.2s',
        '&:hover': {
          transform: 'translateY(-4px)',
          boxShadow: 3
        }
      }}
      onClick={() => navigate(path)}
    >
      <Box sx={{ mb: 2, color: 'primary.main' }}>
        {icon}
      </Box>
      <Typography variant="h6" gutterBottom>
        {title}
      </Typography>
      <Typography variant="body2" color="text.secondary">
        {description}
      </Typography>
    </Paper>
  );
} 