# Database migration script
The following explains how to use the "Migration.ps1" script

## What problem does Migrate.ps1 solve
There are two different ways of handling database deploy:

State vs migration

[Database versioning best practices](http://enterprisecraftsmanship.com/2015/08/10/database-versioning-best-practices/)

[State vs migration-driven database delivery](http://enterprisecraftsmanship.com/2015/08/18/state-vs-migration-driven-database-delivery/)

[Critiquing two different approaches to delivering databases: Migrations vs state](http://workingwithdevs.com/delivering-databases-migrations-vs-state/)



## Migrate.ps1

### Create new version
From powershell run the script "Migrate.ps1" with parameters like follows:

    Migrate.ps1 -newversion "$version"


This creates a new folder with "$version" as name in the folder where "Migrate.ps1" 
are located.

### Create new feature
From powershell run the script "NewFeature.ps1" with parameters like follows:

    NewFeature.ps1 "$newfeaturename"

This creates a new folder with "$newfeaturename" as name in the folder 
where "NewFeature.ps1" are located.

Inside this folder are the following folder structure:
* Upgrade
  *  PreMigrate
  *  Migrate
  *  PostMigrate
  *  Functions
  *  StoreProcedures
  *  Views
  *  Functions



### Upgrade database
From powershell run the script "Migrate.ps1" with parameters like follows:

    Migrate.ps1 "$servername" "$database" "$username" "$password" "$toVersionString"

Where $toVersionString is optional and will default be the latest version.

### Rollback database
From powershell run the script "Migrate.ps1" with parameters like follows:

    Migrate.ps1 -rollback "$servername" "$database" "$username" "$password" "$toVersionString"
