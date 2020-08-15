#!/usr/bin/env bash

base=$1
id=$2

(
echo "delete from presearch_list where category_id = ${id};"
cat ${base}.master | sort | \
	grep -v '^#' | \
	sed -e 's/,/","/' -e 's/^/"/' -e 's/$/");/' | \
	nl -s',' | \
	sed "s/^ */insert into presearch_list values (${id},/" 
) | mysql claytons_index
