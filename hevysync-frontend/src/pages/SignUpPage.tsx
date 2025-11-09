import { SignUp } from '@clerk/clerk-react';
import { useUser } from '@clerk/clerk-react';
import { useNavigate } from 'react-router-dom';
import { useEffect } from 'react';

export default function SignUpPage() {
  const { user, isLoaded } = useUser();
  const navigate = useNavigate();

  useEffect(() => {
    if (isLoaded && user) {
      navigate('/dashboard');
    }
  }, [isLoaded, user, navigate]);

  if (!isLoaded) {
    return <div className="min-h-screen flex items-center justify-center bg-gray-50">Loading...</div>;
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md w-full space-y-8">
        <div>
          <h2 className="mt-6 text-center text-3xl font-extrabold text-gray-900">
            HevySync
          </h2>
          <p className="mt-2 text-center text-sm text-gray-600">
            Create your account to start tracking
          </p>
        </div>

        <div className="bg-white p-8 rounded-lg shadow">
          <SignUp
            appearance={{
              elements: {
                rootBox: "w-full",
                card: "shadow-none",
              },
            }}
            routing="path"
            path="/sign-up"
            signInUrl="/login"
            afterSignUpUrl="/dashboard"
          />
        </div>
      </div>
    </div>
  );
}
