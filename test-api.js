// Test script to verify API endpoints
const API_URL = 'http://localhost:5189';

async function testDemoLogin() {
  console.log('Testing demo login...');
  
  try {
    const response = await fetch(`${API_URL}/login?useCookies=false`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        email: 'demo@hevysync.com',
        password: 'Chicken1234!'
      }),
    });

    if (response.ok) {
      const data = await response.json();
      console.log('✅ Login successful!');
      console.log('Token:', data.accessToken ? 'Present' : 'Missing');
      return data.accessToken;
    } else {
      const errorText = await response.text();
      console.error('❌ Login failed:', response.status, errorText);
      return null;
    }
  } catch (error) {
    console.error('❌ Login error:', error.message);
    return null;
  }
}

async function testGetWorkouts(token) {
  console.log('\nTesting get workouts...');
  
  try {
    const response = await fetch(`${API_URL}/average2savage/workouts`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
    });

    if (response.ok) {
      const data = await response.json();
      console.log('✅ Workouts fetched successfully!');
      console.log('Number of workouts:', data.length);
      
      if (data.length > 0) {
        console.log('\nFirst workout:');
        console.log('- Name:', data[0].Name || data[0].name);
        console.log('- Exercises:', (data[0].Exercises || data[0].exercises)?.length);
        console.log('- Activity Status:', data[0].WorkoutActivity?.Status || data[0].workoutActivity?.status);
        console.log('- Week:', data[0].WorkoutActivity?.Week || data[0].workoutActivity?.week);
        console.log('- Day:', data[0].WorkoutActivity?.Day || data[0].workoutActivity?.day);
        
        console.log('\nFull workout data:');
        console.log(JSON.stringify(data[0], null, 2));
      }
      
      return data;
    } else {
      const errorText = await response.text();
      console.error('❌ Get workouts failed:', response.status, errorText);
      return null;
    }
  } catch (error) {
    console.error('❌ Get workouts error:', error.message);
    return null;
  }
}

async function runTests() {
  const token = await testDemoLogin();
  
  if (token) {
    await testGetWorkouts(token);
  }
}

runTests();
