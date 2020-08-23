#!/usr/bin/env bash

mysql claytons_index <<!
truncate table presearch_category;

load data local infile 'category.master' 
    into table presearch_category
    fields terminated by ','
    optionally enclosed by '"';
!
