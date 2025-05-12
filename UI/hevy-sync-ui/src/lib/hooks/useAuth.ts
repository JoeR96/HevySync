import { useAuth } from '../auth'

export const useAuthHook = () => {
  const { token, isAuthenticated, setToken, logout } = useAuth()

  return {
    token,
    isAuthenticated,
    setToken,
    logout
  }
} 