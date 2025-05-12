import { useState, useEffect } from 'react';
import { 
  Box, 
  Container, 
  Typography, 
  Card,
  CardContent,
  Alert,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  List,
  ListItem,
  ListItemText,
  Chip,
  Divider,
  Grid,
  Paper,
  Button
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import { FitnessCenter as FitnessCenterIcon, Settings as SettingsIcon, Dashboard as DashboardIcon, Timeline as TimelineIcon, Assessment as AssessmentIcon } from '@mui/icons-material';
import { apiClient, HevyWorkout } from '../lib/api';
import { useNavigate } from 'react-router-dom';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { NavButton } from '../components/NavButton';

export function Dashboard() {
  const [isLoadingWorkouts, setIsLoadingWorkouts] = useState(false);
  const [workouts, setWorkouts] = useState<HevyWorkout[]>([]);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  const loadWorkouts = async () => {
    setIsLoadingWorkouts(true);
    setError(null);
    try {
      const since = new Date();
      since.setDate(since.getDate() - 30);
      const response = await apiClient.getWorkouts(since);
      setWorkouts(response.events.map(event => event.workout));
    } catch (err) {
      setError('Failed to load workouts. Please try again.');
    } finally {
      setIsLoadingWorkouts(false);
    }
  };

  useEffect(() => {
    loadWorkouts();
  }, []);

  const formatDuration = (startTime: string, endTime: string) => {
    try {
      const start = new Date(startTime);
      const end = new Date(endTime);
      const duration = end.getTime() - start.getTime();
      const minutes = Math.round(duration / 1000 / 60);
      return `${minutes} minutes`;
    } catch (error) {
      console.error('Error formatting duration:', error);
      return 'Duration unavailable';
    }
  };

  const formatDate = (dateString: string) => {
    try {
      const date = new Date(dateString);
      return date.toLocaleDateString('en-US', {
        weekday: 'long',
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        timeZone: 'UTC'
      });
    } catch (error) {
      console.error('Error formatting date:', error);
      return 'Date unavailable';
    }
  };

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Box sx={{ display: 'flex', justifyContent: 'center', mb: 4 }}>
        <Typography
          variant="h3"
          component="h1"
          align="center"
          gutterBottom
          sx={{
            fontWeight: 700,
            background: 'linear-gradient(45deg, #1976d2, #21CBF3)',
            WebkitBackgroundClip: 'text',
            WebkitTextFillColor: 'transparent',
            mb: 2
          }}
        >
          Dashboard
        </Typography>
      </Box>

      <Grid container spacing={3}>
        <Grid item xs={12} sm={6} md={4}>
          <NavButton
            title="Workouts"
            icon={<FitnessCenterIcon sx={{ fontSize: 48 }} />}
            path="/workouts"
            description="View and manage your workout history"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={4}>
          <NavButton
            title="Analytics"
            icon={<TimelineIcon sx={{ fontSize: 48 }} />}
            path="/analytics"
            description="Track your progress and performance"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={4}>
          <NavButton
            title="Reports"
            icon={<AssessmentIcon sx={{ fontSize: 48 }} />}
            path="/reports"
            description="Generate detailed workout reports"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={4}>
          <NavButton
            title="Settings"
            icon={<SettingsIcon sx={{ fontSize: 48 }} />}
            path="/settings"
            description="Configure your preferences and API keys"
          />
        </Grid>
      </Grid>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      {isLoadingWorkouts ? (
        <LoadingSpinner message="Loading workouts..." />
      ) : workouts.length === 0 ? (
        <Box sx={{ 
          display: 'flex', 
          flexDirection: 'column',
          alignItems: 'center', 
          justifyContent: 'center', 
          py: 8,
          gap: 2
        }}>
          <Typography variant="h6" color="text.secondary">
            No workouts found
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Try adjusting your date range or check your API key settings
          </Typography>
        </Box>
      ) : (
        <Grid container spacing={2}>
          {workouts.map((workout) => (
            <Grid item xs={12} sm={6} md={4} key={workout.id}>
              <Card sx={{ height: '100%' }}>
                <CardContent sx={{ p: 2 }}>
                  <Box sx={{ mb: 1 }}>
                    <Typography variant="h6" gutterBottom sx={{ mb: 0.5 }}>
                      {workout.title}
                    </Typography>
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 0.5 }}>
                      {formatDate(workout.start_time)}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Duration: {formatDuration(workout.start_time, workout.end_time)}
                    </Typography>
                  </Box>

                  {workout.description && (
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                      {workout.description}
                    </Typography>
                  )}

                  <Divider sx={{ my: 1 }} />

                  {workout.exercises.map((exercise, index) => (
                    <Accordion key={index} sx={{ mb: 0.5 }}>
                      <AccordionSummary expandIcon={<ExpandMoreIcon />} sx={{ minHeight: '48px' }}>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, width: '100%' }}>
                          <Typography variant="subtitle2" sx={{ flex: 1 }}>
                            {exercise.title}
                          </Typography>
                          <Chip 
                            label={`${exercise.sets.length} sets`}
                            size="small"
                            color="primary"
                            variant="outlined"
                          />
                        </Box>
                      </AccordionSummary>
                      <AccordionDetails sx={{ p: 1 }}>
                        <List dense sx={{ py: 0 }}>
                          {exercise.sets.map((set, setIndex) => (
                            <ListItem key={setIndex} sx={{ py: 0.5 }}>
                              <ListItemText
                                primary={
                                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                    <Typography variant="body2" sx={{ minWidth: '40px' }}>
                                      Set {setIndex + 1}
                                    </Typography>
                                    <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                                      <Typography variant="body2" color="text.primary">
                                        {set.weight_kg}kg
                                      </Typography>
                                      <Typography variant="body2" color="text.primary">
                                        {set.reps} reps
                                      </Typography>
                                      {set.distance_meters && (
                                        <Typography variant="body2" color="text.primary">
                                          {set.distance_meters}m
                                        </Typography>
                                      )}
                                      {set.duration_seconds && (
                                        <Typography variant="body2" color="text.primary">
                                          {Math.round(set.duration_seconds / 60)}min
                                        </Typography>
                                      )}
                                      {set.rpe && (
                                        <Typography variant="body2" color="text.primary">
                                          RPE {set.rpe}
                                        </Typography>
                                      )}
                                    </Box>
                                  </Box>
                                }
                              />
                            </ListItem>
                          ))}
                          {exercise.notes && (
                            <ListItem sx={{ py: 0.5 }}>
                              <ListItemText
                                secondary={
                                  <Typography variant="body2" color="text.secondary" sx={{ fontStyle: 'italic' }}>
                                    Notes: {exercise.notes}
                                  </Typography>
                                }
                              />
                            </ListItem>
                          )}
                        </List>
                      </AccordionDetails>
                    </Accordion>
                  ))}
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      )}
    </Container>
  );
}