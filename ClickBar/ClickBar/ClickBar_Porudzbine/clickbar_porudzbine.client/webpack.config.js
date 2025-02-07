const path = require('path');

module.exports = {
    entry: './src/index.js', // Ulazna tačka vaše aplikacije
    output: {
        filename: 'bundle.js', // Ime izlaznog fajla
        path: path.resolve(__dirname, 'dist'), // direktorijum za izlazne fajlove
    },
    module: {
        rules: [
            {
                test: /\.jsx?$/, // Pravilo za .js i .jsx fajlove
                exclude: /node_modules/,
                use: {
                    loader: 'babel-loader',
                    options: {
                        presets: ['@babel/preset-env', '@babel/preset-react'],
                    },
                },
            },
            {
                test: /\.css$/, // Pravilo za .css fajlove
                use: ['style-loader', 'css-loader'],
            },
        ],
    },
    devServer: {
        contentBase: path.join(__dirname, 'public'), // direktorijum za statičke fajlove
        port: 3000, // Port na kojem će dev server raditi
        hot: true, // Omogućava hot module replacement
        open: true, // Automatski otvara stranicu u pregledaču
    },
};