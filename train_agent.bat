@echo off

set startTime=%time%

set startYear=%date:~10,4%
set startmonth=%date:~4,2%
set startDay=%date:~7,2%
set startDate=%startYear%-%startMonth%-%startDay%
 
:: Trim leading spaces from the time
for /f "tokens=* delims= " %%a in ("%startTime%") do set startTime=%%a

:: Replace forward slashes in the date with underscores
set startDate=%startDate:/=_%

:: Replace colons in the time with hypens
set startTime=%startTime::=-%

:: Replace full stops in the time with hypens
set startTime=%startTime:.=-%

start /b tensorboard --logdir=results\%startDate%_%startTime%
mlagents-learn config/ppo/roguelike.yaml --run-id=%startDate%_%startTime%
