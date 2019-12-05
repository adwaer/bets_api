start runner.nats.cmd
popd

ping 127.0.0.1 -n 6 > nul

start runner.master.cmd
popd

start runner.slave.cmd
popd