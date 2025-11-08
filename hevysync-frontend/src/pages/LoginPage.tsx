import { SignIn, useSignIn } from '@clerk/clerk-react';
import { useNavigate } from 'react-router-dom';
import { useState } from 'react';

export default function LoginPage() {
  const { signIn, setActive } = useSignIn();
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(false);

  const handleDemoLogin = async () => {
    if (!signIn) return;

    setIsLoading(true);
    try {
      const result = await signIn.create({
        identifier: 'demo@hevysync.com',
        password: 'Demo123!',
      });

      if (result.status === 'complete') {
        await setActive({ session: result.createdSessionId });
        navigate('/dashboard');
      }
    } catch (error) {
      console.error('Demo login failed:', error);
      alert('Demo login failed. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <div className="max-w-md w-full space-y-8">
        <div>
          <h2 className="mt-6 text-center text-3xl font-extrabold text-gray-900">
            HevySync
          </h2>
          <p className="mt-2 text-center text-sm text-gray-600">
            Track your workout progress
          </p>
        </div>

        <div className="bg-white p-8 rounded-lg shadow space-y-6">
          <SignIn
            routing="path"
            path="/login"
            signUpUrl="/sign-up"
            afterSignInUrl="/dashboard"
          />

          <div className="relative">
            <div className="absolute inset-0 flex items-center">
              <div className="w-full border-t border-gray-300" />
            </div>
            <div className="relative flex justify-center text-sm">
              <span className="px-2 bg-white text-gray-500">Or</span>
            </div>
          </div>

          <button
            onClick={handleDemoLogin}
            disabled={isLoading}
            className="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isLoading ? 'Logging in...' : 'Demo Login'}
          </button>
          <p className="text-xs text-center text-gray-500">
            Email: demo@hevysync.com | Password: Demo123!
          </p>
        </div>
      </div>
    </div>
  );
}
