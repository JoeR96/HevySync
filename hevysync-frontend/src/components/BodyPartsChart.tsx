import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip } from 'recharts';
import { Target } from 'lucide-react';
import { DashboardCard } from './DashboardCard';

interface BodyPartData {
  name: string;
  value: number;
}

interface BodyPartsChartProps {
  data: BodyPartData[];
}

const COLORS = ['#6366f1', '#8b5cf6', '#ec4899', '#f59e0b', '#10b981'];

export function BodyPartsChart({ data }: BodyPartsChartProps) {
  return (
    <DashboardCard>
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-lg font-bold text-gray-900 dark:text-white flex items-center">
          <Target className="h-5 w-5 mr-2 text-indigo-600 dark:text-indigo-400" />
          Body Parts
        </h3>
      </div>
      {data.length > 0 ? (
        <ResponsiveContainer width="100%" height={200}>
          <PieChart>
            <Pie
              data={data}
              cx="50%"
              cy="50%"
              labelLine={false}
              label={({ name, value }) => `${name}: ${value}`}
              outerRadius={70}
              fill="#8884d8"
              dataKey="value"
            >
              {data.map((_entry: any, index: number) => (
                <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
              ))}
            </Pie>
            <Tooltip 
              contentStyle={{ 
                backgroundColor: '#313131', 
                border: '1px solid #454545', 
                borderRadius: '8px',
                color: '#fff',
                fontWeight: 600
              }}
            />
          </PieChart>
        </ResponsiveContainer>
      ) : (
        <div className="text-center py-8 text-gray-600 dark:text-dark-text font-semibold">
          No exercises to display
        </div>
      )}
    </DashboardCard>
  );
}
