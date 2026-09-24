CREATE DATABASE IF NOT EXISTS buildtruck_users        CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE IF NOT EXISTS buildtruck_projects      CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE IF NOT EXISTS buildtruck_documentation CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE IF NOT EXISTS buildtruck_incidents     CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE IF NOT EXISTS buildtruck_machinery     CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE IF NOT EXISTS buildtruck_materials     CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE IF NOT EXISTS buildtruck_personnel     CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE IF NOT EXISTS buildtruck_configurations CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE IF NOT EXISTS buildtruck_notifications  CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE IF NOT EXISTS buildtruck_stats         CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE IF NOT EXISTS buildtruck_main          CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

GRANT ALL PRIVILEGES ON buildtruck_users.*          TO 'buildtruckuser'@'%';
GRANT ALL PRIVILEGES ON buildtruck_projects.*       TO 'buildtruckuser'@'%';
GRANT ALL PRIVILEGES ON buildtruck_documentation.*  TO 'buildtruckuser'@'%';
GRANT ALL PRIVILEGES ON buildtruck_incidents.*      TO 'buildtruckuser'@'%';
GRANT ALL PRIVILEGES ON buildtruck_machinery.*      TO 'buildtruckuser'@'%';
GRANT ALL PRIVILEGES ON buildtruck_materials.*      TO 'buildtruckuser'@'%';
GRANT ALL PRIVILEGES ON buildtruck_personnel.*      TO 'buildtruckuser'@'%';
GRANT ALL PRIVILEGES ON buildtruck_configurations.* TO 'buildtruckuser'@'%';
GRANT ALL PRIVILEGES ON buildtruck_notifications.*  TO 'buildtruckuser'@'%';
GRANT ALL PRIVILEGES ON buildtruck_stats.*          TO 'buildtruckuser'@'%';
GRANT ALL PRIVILEGES ON buildtruck_main.*           TO 'buildtruckuser'@'%';

FLUSH PRIVILEGES;
