const path = require('path');
const webpack = require('webpack');
const TerserPlugin = require('terser-webpack-plugin');
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const OptimizeCSSAssetsPlugin = require("optimize-css-assets-webpack-plugin");

function getClientConfig(env) {
    const isDevBuild = !(env && env.prod);
    return {
        stats: { modules: false },
        mode: isDevBuild ? 'development' : 'production',
        resolve: {
            extensions: ['*', '.js']
        },
        output: {
            path: path.join(__dirname, 'wwwroot', 'dist'),
            publicPath: 'dist/',
            filename: '[name].js',
            library: '[name]_[hash]',
            globalObject: 'this'
        },
        module: {
            rules: [
                { test: /\.(woff|woff2|eot|ttf)(\?v=\d+\.\d+\.\d+)?$/, use: 'url-loader?limit=25000' },
                { test: /\.(png|jpg|jpeg|gif|svg)$/, use: 'url-loader?limit=25000' },
                {
                    test: /\.css(\?|$)/,
                    use: [
                        MiniCssExtractPlugin.loader,
                        { loader: 'css-loader', options: { importLoaders: 1 } },
                        'postcss-loader'
                    ]
                },
                {
                    test: /\.scss(\?|$)/,
                    use: [
                        MiniCssExtractPlugin.loader,
                        { loader: 'css-loader', options: { importLoaders: 2 } },
                        'postcss-loader',
                        'sass-loader'
                    ]
                }
            ]
        },
        entry: {
            vendor: [
                '@aspnet/signalr', '@aspnet/signalr-protocol-msgpack', 'buffer', 'domhandler', 'htmlparser2', 'lodash-es',
                'mobx', 'mobx-react', 'react', 'react-dom', 'react-hotkeys', 'react-overlays', 'react-transition-group'
            ]
        },
        plugins: [
            new webpack.DllPlugin({
                path: path.join(__dirname, 'wwwroot', 'dist', '[name]-manifest.json'),
                name: '[name]_[hash]'
            }),
            new MiniCssExtractPlugin({ filename: 'vendor.css' })
        ],
        optimization: {
            concatenateModules: true,
            noEmitOnErrors: true,
            splitChunks: {
                cacheGroups: {
                    styles: {
                        name: 'styles',
                        test: /\.css$/,
                        chunks: 'all',
                        enforce: true
                    }
                }
            },
            minimizer: isDevBuild
                ? []
                : [
                    new TerserPlugin({
                        terserOptions: {
                            ecma: 6
                        }
                    }),
                    new OptimizeCSSAssetsPlugin({})
                ]
        }
    };
}

module.exports = (env) => { return [getClientConfig(env)]; };