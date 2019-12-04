# Filer
Command line utiliy that performs various mildly useful tasks in a file system.

# Usage

## concat
Concatenates multiple files

```
filer concat *.sql -s "\ngo\n" > output.sql
filer concat *.sql !create.sql -s "\ngo\n" > output.sql
```

## delimiter
Changes delimiter in a csv file

```
filer delimiter semicolon.csv ";" "," > comma.csv
filer delimiter tab.csv "\t" "|" > pipe.csv
