import { Trophy, TrendingUp } from 'lucide-react';
import { DashboardCard } from './DashboardCard';

interface OneRepMaxData {
  exerciseName: string;
  weight: number;
  reps: number;
  estimatedMax?: number;
  date?: string;
}

interface OneRepMaxChartProps {
  data: OneRepMaxData[];
}

// Epley formula: 1RM = weight × (1 + reps/30)
function calculateOneRepMax(weight: number, reps: number): number {
  if (reps === 1) return weight;
  return Math.round(weight * (1 + reps / 30) * 2) / 2; // Round to nearest 0.5kg
}

export function OneRepMaxChart({ data }: OneRepMaxChartProps) {
  // Calculate 1RM for each exercise
  const oneRepMaxes = data.map(item => ({
    ...item,
    estimatedMax: item.estimatedMax || calculateOneRepMax(item.weight, item.reps)
  }));

  // Sort by estimated max descending
  const sortedMaxes = oneRepMaxes.sort((a, b) => b.estimatedMax - a.estimatedMax);

  return (
    <DashboardCard>
      <div className="flex items-center justify-between mb-3">
        <div className="flex items-center">
          <Trophy className="h-5 w-5 mr-2 text-indigo-600 dark:text-indigo-400" />
          <h3 className="text-lg font-bold text-gray-900 dark:text-white">
            Estimated 1RM
          </h3>
        </div>
      </div>

      {sortedMaxes.length === 0 ? (
        <div className="text-center py-4 text-gray-500 dark:text-gray-400">
          <Trophy className="h-8 w-8 mx-auto mb-2 opacity-50" />
          <p className="text-xs">No workout data yet</p>
        </div>
      ) : (
        <div className="space-y-2">
          {sortedMaxes.slice(0, 5).map((item, index) => (
            <div 
              key={index}
              className="flex items-center justify-between p-2 bg-gray-50 dark:bg-gray-700/50 rounded hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
            >
              <div className="flex-1 min-w-0">
                <div className="font-semibold text-gray-900 dark:text-white text-xs truncate">
                  {item.exerciseName}
                </div>
                <div className="text-xs text-gray-600 dark:text-gray-400 font-semibold">
                  {item.weight}kg × {item.reps}
                </div>
              </div>
              <div className="text-right ml-2">
                <div className="text-xl font-bold text-indigo-600 dark:text-indigo-400">
                  {item.estimatedMax}
                  <span className="text-xs ml-0.5">kg</span>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      <div className="mt-3 pt-2 border-t border-gray-200 dark:border-gray-700">
        <p className="text-xs text-gray-500 dark:text-gray-400 text-center font-semibold">
          Epley: 1RM = weight × (1 + reps ÷ 30)
        </p>
      </div>
    </DashboardCard>
  );
}
