Overlook
========

Open source system to monitor system metrics and graph the data for long term analysis.

Background
----------

Overlook was created because I could not find a good (free at least) piece of software that could answer questions about my computer's performance such as "How has the average daily temperature changed over the past month while playing Borderlands".  Most temperature and metric monitoring applications only keep track of sensor data in memory, and once you close the application or shut down your data is gone.  

Components
----------

Overlook consists of two main components:

* Server/system tray application that takes snapshots of all sensor data and all running applications and saves them to an embedded database system.  By default, snapshots are taken every 15 seconds, but is configurable via the application's config file.  The server also consists of an embedded web server (via NancyFX) which is used to retrieve information about saved metric data.

* GUI application written with WPF, which queries the server for available metrics and data.  When multiple metrics are selected it takes all the data, removes any data at timestamps that are not valid for all selected metrics, and averages out the data on a daily basis.  It then plots the data on an Oxyplot graph, allowing you to visually see the metric change over time.

Metrics
-------

The server uses the library from the [Open Hardware Monitor Project](http://openhardwaremonitor.org/).  Any sensor that the Open Hardware Monitor project supports will be automatically saved to the database.  
