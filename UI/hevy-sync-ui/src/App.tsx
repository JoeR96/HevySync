import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom'
import { ThemeProvider, createTheme, CssBaseline } from '@mui/material'
import { useThemeStore } from './stores/theme'
import { Login } from './pages/Login'
import { Dashboard } from './pages/Dashboard'
import { Settings } from './pages/Settings'
import { Workouts } from './pages/Workouts'
import { AppBar, Toolbar, Typography, IconButton, Box, Button } from '@mui/material'
import { FitnessCenter as FitnessCenterIcon, Settings as SettingsIcon, Dashboard as DashboardIcon, Logout as LogoutIcon } from '@mui/icons-material'
import { useAuth } from './lib/auth'

function App() {
  const { isDarkMode } = useThemeStore()
  const { isAuthenticated, logout } = useAuth()

  const theme = createTheme({
    palette: {
      mode: isDarkMode ? 'dark' : 'light',
      primary: {
        main: '#1976d2',
      },
      secondary: {
        main: '#21CBF3',
      },
    },
  })

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Router>
        <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
          <AppBar position="static">
            <Toolbar>
              <Box sx={{ display: 'flex', alignItems: 'center', flex: 1 }}>
                <FitnessCenterIcon sx={{ mr: 2 }} />
                <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
                  HevySync
                </Typography>
                {isAuthenticated && (
                  <>
                    <Button
                      color="inherit"
                      startIcon={<DashboardIcon />}
                      href="/dashboard"
                    >
                      Dashboard
                    </Button>
                    <IconButton
                      color="inherit"
                      href="/settings"
                    >
                      <SettingsIcon />
                    </IconButton>
                    <IconButton
                      color="inherit"
                      onClick={logout}
                    >
                      <LogoutIcon />
                    </IconButton>
                  </>
                )}
              </Box>
            </Toolbar>
          </AppBar>

          <Box sx={{ flex: 1, py: 4 }}>
            <Routes>
              <Route path="/login" element={<Login />} />
              <Route
                path="/dashboard"
                element={
                  isAuthenticated ? <Dashboard /> : <Navigate to="/login" replace />
                }
              />
              <Route
                path="/workouts"
                element={
                  isAuthenticated ? <Workouts /> : <Navigate to="/login" replace />
                }
              />
              <Route
                path="/settings"
                element={
                  isAuthenticated ? <Settings /> : <Navigate to="/login" replace />
                }
              />
              <Route path="/" element={<Navigate to="/dashboard" replace />} />
            </Routes>
          </Box>
        </Box>
      </Router>
    </ThemeProvider>
  )
}

export default App
