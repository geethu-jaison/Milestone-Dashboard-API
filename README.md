# \# iConnect dashboard API gateway (.NET 9)

# 

# This project is a separate ASP.NET Core Web API service for multi-site support.

# 

# It runs independently from the Milestone plugin and is used by the parent site to fetch child site summary data.

# 

# \## Purpose

# 

# \- Expose child-site dashboard data in a normalized format

# \- Read data from the plugin database using Dapper

# \- Support secure parent-to-child data pull

# \- Run reliably as a Windows Service

# 

# \## Tech Stack

# 

# \- .NET 9 (`net9.0`)

# \- ASP.NET Core Web API

