import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import { Target } from 'lucide-react';
import { DashboardCard } from './DashboardCard';

interface ProgressData {
  date: string;
  weight: number;
}

interface ExerciseProgressChartProps {
  exerciseName: string;
  trainingMax: number;
  data: ProgressData[];
}

export function ExerciseProgressChart({ exerciseName, trainingMax, data }: ExerciseProgressChartProps) {
  return (
    <DashboardCard className="flex flex-col h-full">
      <div className="flex items-center justify-between mb-3">
        <div>
          <h3 className="text-lg font-bold text-gray-900 dark:text-white flex items-center">
            <Target className="h-5 w-5 mr-2 text-indigo-600 dark:text-indigo-400" />
            {exerciseName} Progress
          </h3>
          <p className="text-sm text-gray-600 dark:text-gray-400 mt-1 font-semibold">Training Max: {trainingMax} kg</p>
        </div>
      </div>
      <div className="flex-1 min-h-0">
        <ResponsiveContainer width="100%" height="100%">
        <LineChart data={data}>
          <CartesianGrid strokeDasharray="3 3" stroke="#454545" />
          <XAxis 
            dataKey="date" 
            stroke="#818181" 
            tick={{ fill: '#818181', fontSize: 11, fontWeight: 600 }}
            angle={-45}
            textAnchor="end"
            height={60}
          />
          <YAxis 
            stroke="#818181" 
            tick={{ fill: '#818181', fontSize: 12, fontWeight: 600 }}
            domain={['dataMin - 5', 'dataMax + 5']}
          />
          <Tooltip
            contentStyle={{ 
              backgroundColor: '#313131', 
              border: '1px solid #454545', 
              borderRadius: '8px',
              color: '#fff',
              fontWeight: 600
            }}
            formatter={(value: any) => [`${value} kg`, 'Weight']}
          />
          <Line
            type="monotone"
            dataKey="weight"
            stroke="#6366f1"
            strokeWidth={3}
            dot={{ fill: '#6366f1', r: 4 }}
            activeDot={{ r: 6 }}
          />
        </LineChart>
        </ResponsiveContainer>
      </div>
    </DashboardCard>
  );
}
