Command line program for running PREACT (which is the core of WUInity).
If running a single simulation the only parameter that is required is the *.wui file that should run.
If running multiple cases the following parameters are required:
- File to run (string)
- Number of runs (integer): the maximum amount of simulations to run, can be lower if convergence is met (less than 2% difference in the RSET for 10 simulations in sequence by default)
- Batch size (integer): This number determines how many parallel processes (simulations) will run at the same time, so do not set a number above your CPU thread count. Each batch is synchronized before starting the next batch and checks for convergence (if met, ends run).
- Simulation number offset: Should basically always be set to 0, this is mainly used by the automatic runs to keep track of output data naming.