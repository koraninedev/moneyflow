Run in order against SQL Server:

```powershell
sqlcmd -S <server> -d master -i 001_create_database.sql
sqlcmd -S <server> -i 002_create_tables.sql
sqlcmd -S <server> -i 003_create_indexes.sql
sqlcmd -S <server> -i 004_seed_data.sql
sqlcmd -S <server> -i 006_user_pay_cycle.sql
```

Demo login after seed: `demo@moneyflow.app` / `Demo123!`

Clear demo seed only (keeps other users/tables):

```powershell
sqlcmd -S <server> -i 005_clear_seed_data.sql
```
