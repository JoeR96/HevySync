-- Delete demo user and related data
DELETE FROM "Activities" WHERE "UserId" IN (SELECT "Id" FROM "AspNetUsers" WHERE "Email" = 'demo@hevysync.com');
DELETE FROM "Workouts" WHERE "UserId" IN (SELECT "Id" FROM "AspNetUsers" WHERE "Email" = 'demo@hevysync.com');
DELETE FROM "AspNetUsers" WHERE "Email" = 'demo@hevysync.com';
