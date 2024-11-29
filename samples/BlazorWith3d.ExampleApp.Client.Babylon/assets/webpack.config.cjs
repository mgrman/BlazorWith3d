const path = require('path');

module.exports = env => {

    return {
        entry: './src/app.ts',
        devtool: env && env.production ? 'none' : 'source-map',
        module: {
            rules: [
                {
                    test: /\.tsx?$/,
                    use: 'ts-loader',
                    exclude: /node_modules/,
                },
            ],
        },experiments: {
            outputModule: true,
        },
        resolve: {
            extensions: ['.tsx', '.ts', '.js'],
        },
        output: {
            filename: "bundle.js",
            path: path.resolve(__dirname, "dist"),
            library: {
                type: "module",
            },
        },
        optimization: {
            minimize: false
        },
    };
};