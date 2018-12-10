const path = require('path');
const webpack = require('webpack');
const CheckerPlugin = require('awesome-typescript-loader').CheckerPlugin;
const UglifyJsPlugin = require('uglifyjs-webpack-plugin');
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const OptimizeCSSAssetsPlugin = require("optimize-css-assets-webpack-plugin");
const bundleOutputDir = './wwwroot/dist';

function getClientConfig(env) {
    const isDevBuild = !(env && env.prod);
    return {
        stats: { modules: false },
        mode: isDevBuild ? 'development' : 'production',
        devtool: isDevBuild ? 'eval-source-map' : 'source-map',
        resolve: { extensions: ['.js', '.jsx', '.ts', '.tsx'] },
        entry: { 'main': './ClientApp/boot.tsx' },
        output: {
            path: path.join(__dirname, bundleOutputDir),
            filename: '[name].js',
            publicPath: 'dist/',
            globalObject: 'this'
        },
        module: {
            rules: [
                { test: /\.tsx?$/, include: /ClientApp/, use: 'awesome-typescript-loader?silent=true' },
                { test: /\.(woff|woff2|eot|ttf)(\?v=\d+\.\d+\.\d+)?$/, use: 'url-loader?limit=25000' },
                { test: /\.(png|jpg|jpeg|gif|svg)$/, use: 'url-loader?limit=25000' },
                {
                    test: /\.css(\?|$)/,
                    use: isDevBuild
                        ? [
                            'style-loader',
                            {
                                loader: 'typings-for-css-modules-loader',
                                options: {
                                    sourceMap: true, modules: true, namedExport: true, camelCase: 'dashesOnly', importLoaders: 2
                                }
                            },
                            {
                                loader: 'postcss-loader',
                                options: { sourceMap: true, plugins: () => [require('autoprefixer')()] }
                            },
                            { loader: 'resolve-url-loader', options: { sourceMap: true } }
                        ]
                        : [
                            MiniCssExtractPlugin.loader,
                            {
                                loader: 'typings-for-css-modules-loader',
                                options: {
                                    sourceMap: true, minimize: true, modules: true, namedExport: true, camelCase: 'dashesOnly', importLoaders: 2
                                }
                            },
                            {
                                loader: 'postcss-loader',
                                options: { sourceMap: true, plugins: () => [require('autoprefixer')()] }
                            },
                            { loader: 'resolve-url-loader', options: { sourceMap: true } }
                        ]
                },
                {
                    test: /\.scss(\?|$)/,
                    use: isDevBuild
                        ? [
                            'style-loader',
                            {
                                loader: 'typings-for-css-modules-loader',
                                options: {
                                    sourceMap: true, modules: true, namedExport: true, camelCase: 'dashesOnly', importLoaders: 4
                                }
                            },
                            { loader: 'postcss-loader',options: { sourceMap: true } },
                            { loader: 'resolve-url-loader', options: { sourceMap: true } },
                            { loader: 'sass-loader', options: { sourceMap: true } },
                            {
                                loader: 'sass-resources-loader',
                                options: {
                                    resources: './ClientApp/styles/sass-resources.scss'
                                }
                            }
                        ]
                        : [
                            MiniCssExtractPlugin.loader,
                            {
                                loader: 'typings-for-css-modules-loader',
                                options: {
                                    sourceMap: true, minimize: true, modules: true, namedExport: true, camelCase: 'dashesOnly', importLoaders: 4
                                }
                            },
                            { loader: 'postcss-loader', options: { sourceMap: true } },
                            { loader: 'resolve-url-loader', options: { sourceMap: true } },
                            { loader: 'sass-loader', options: { sourceMap: true } },
                            {
                                loader: 'sass-resources-loader',
                                options: {
                                    resources: './ClientApp/styles/sass-resources.scss'
                                }
                            }
                        ]
                }
            ]
        },
        plugins: [
            new webpack.DllReferencePlugin({
                context: __dirname,
                manifest: require('./wwwroot/dist/vendor-manifest.json')
            }),
            new CheckerPlugin(),
            require('autoprefixer')
        ].concat(isDevBuild
            ? [
                new webpack.SourceMapDevToolPlugin({
                    filename: '[file].map',
                    moduleFilenameTemplate:
                        path.relative(bundleOutputDir,
                            '[resourcePath]') // Point sourcemap entries to the original file locations on disk
                })
            ]
            : [new MiniCssExtractPlugin({ filename: 'site.css' })]),
        optimization: {
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
                    new UglifyJsPlugin({
                        uglifyOptions: {
                            ecma: 6
                        },
                        cache: true,
                        parallel: true,
                        sourceMap: true
                    }),
                    new OptimizeCSSAssetsPlugin({})
                ]
        }
    };
}

module.exports = (env) => { return [getClientConfig(env)]; };