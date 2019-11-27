# Concat
Commandline utility that concatenates files in a directory

# Usage

```
concat *.sql -s "\ngo\n" > output.sql
concat *.sql !create.sql -s "\ngo\n" > output.sql
```
