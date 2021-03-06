## What problem does DbMigrator try to solve
There are two different ways of handling database deploy: State and migration

### Migration based
A lot of frameworks have migrations build in. But the problems seems to be that they only support 
a subset of the functionality we need. Typically only schemas and indexes.
For example Entity framework have migrations baked in, but it only supports schema and index migrations.
Though you can write your own migrations in pure SQL and put them next to the autogenerated cs files. 
This are though not desireble for us.

### State based
Microsoft has SQL Server Data Tools(SSDT) which support a state based approach. 
The problem with state based approach are if we want to do some kind of data migrations in a release. 
This is possible in SSDT, but the data migration get "bound" to that release. 
So if you update your database more then 1 releases, you have to apply all the release between sequantially.
Then it basicly also becomes migration based.

### Other problems
Someone makes untracked changes in database, making features "works on my machine"
Someone goes back and fixes an issue in a script that there's already been deployed


#### Inspiration / Recommended reading
[Database versioning best practices](http://enterprisecraftsmanship.com/2015/08/10/database-versioning-best-practices/)

[State vs migration-driven database delivery](http://enterprisecraftsmanship.com/2015/08/18/state-vs-migration-driven-database-delivery/)

[Critiquing two different approaches to delivering databases: Migrations vs state](http://workingwithdevs.com/delivering-databases-migrations-vs-state/)

## Features
* Diff
* Upgrade
* Rollback
* Validation

### Why DBMigrator
Based on the above we choose to go the Migration based path.

<!--- Describe why we for example don't use flyway -->

## How it works
Normally migrations are straight sequential list of migrations that get run one after another.
DBMigrator works a bit different, because scripts are organized in features and the again organized in releases.
This organization helps the developer in getting a better overview of the migrations and also helps map it to organizationel releases and features.

Organizing scripts into features also helps to move a feature from one release to another, should it get cut.

### Folder structure and scripts
Folder structure is as follows: 

    <ReleaseVersion>/<Feature>/Migrations/

Example:

    1.0/US_12345/Migrations/

Note that DBMigrator should be at the same place as \<ReleaseVersion\> folder or folder should be referenced by the -f paramether.

Note that versions should not contain leading zeroes (1.0, 1.10 is ok while 1.00 and 1.01 is not)

Script files should be named as follows:

	<order>_<name>.sql

Example:

	1_Test.sql

Rollback script files should be named as follows:

	<order>_rollback_<name>.sql

Example:

	1_rollback_Test.sql


### Work flow
Organization plans upcomming release 2.0
Dev creates 2.0 folder in source code repository
Dev starts implementing feature: AwesomeFeatureX
Dev creates folder AwesomeFeatureX/Migrations under 2.0 folder in source code repository
Scripts are then added

Dev uses DBMigrator on his dev database
When it's time to deploy DBMigrator are used as well

DBMigrator sees to via integrity checks of the database that:
No one has change tables, coloumns, view, indexes, triggers, etc. manually. So changes are always well tracked and consistently applied across environments.
It also sees to that no one goes back and changes in files that's already been deployed.


### Upgrade database
Run DBMigrator with parameters like follows:

    upgrade -v {toVersionString} -s {servername} -d {database} -u {username} -p {password} -f {folderPath}

Where "-v" is optional and will default be the latest version.
If -u and -p is omitted "Integrated Security" will be used

### Downgrade database
Run DBMigrator with parameters like follows:

    downgrade -v {toVersionString} -s {servername} -d {database} -u {username} -p {password} -f {folderPath}
If -u and -p is omitted "Integrated Security" will be used

### TODO
- [ ] Publish dbmigrator as nuget
- [ ] Create Team Services extension for dbmigrator
- [ ] Make DBMigrator create migration scripts with integrity checks, etc.