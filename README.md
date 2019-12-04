# Filer
Command line utiliy that performs various mildly useful tasks in a file system.

## Usage

### concat
Concatenates multiple files, optionally including a separator (`-s`) between each file and/or a header (`-h`) before each file.

Use `\n` for new lines and `\t` for tabs.

```
filer concat *.sql -s "\ngo\n" > output.sql
filer concat *.sql !create.sql -s "\ngo\n" -h "\n--{0}\n" > output.sql
```

### delimiter
Changes delimiter in a csv file.

Use `\t` to specify tab delimiter.

```
filer delimiter semicolon.csv ";" "," > comma.csv
filer delimiter tab.csv "\t" "|" > pipe.csv
