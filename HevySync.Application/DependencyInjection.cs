using HevySync.Application.Common;
using HevySync.Application.DTOs;
using HevySync.Application.Workouts.Commands.CompleteWorkoutDay;
using HevySync.Application.Workouts.Commands.CreateWorkout;
using HevySync.Application.Workouts.Commands.GenerateNextWeek;
using HevySync.Application.Workouts.Commands.GenerateWeekOne;
using HevySync.Application.Workouts.Queries.GetWorkout;
using Microsoft.Extensions.DependencyInjection;

namespace HevySync.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register command handlers
        services.AddScoped<ICommandHandler<CreateWorkoutCommand, WorkoutDto>, CreateWorkoutCommandHandler>();
        services.AddScoped<ICommandHandler<GenerateWeekOneCommand, Dictionary<int, List<SessionExerciseDto>>>, GenerateWeekOneCommandHandler>();
        services.AddScoped<ICommandHandler<CompleteWorkoutDayCommand, CompleteWorkoutDayResult>, CompleteWorkoutDayCommandHandler>();
        services.AddScoped<ICommandHandler<GenerateNextWeekCommand, Dictionary<int, List<SessionExerciseDto>>>, GenerateNextWeekCommandHandler>();

        // Register query handlers
        services.AddScoped<IQueryHandler<GetWorkoutQuery, WorkoutDto?>, GetWorkoutQueryHandler>();

        return services;
    }
}

