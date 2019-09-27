const path = require('path');
const http = require('http');
const express = require('express');
const socketIO = require('socket.io');

const app = express();
const server = http.createServer(app);
const io = socketIO(server);

const publicPath = path.join(__dirname, 'UI');
const port = process.env.PORT || 80;

app.use(express.static(publicPath));

io.on('connect', (socket) => {
    console.log('New user connected!');

    socket.on('join', (params, callback) => {

    });
});

server.listen(port, () => {

});